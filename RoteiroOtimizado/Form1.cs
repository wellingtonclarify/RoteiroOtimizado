using System;
using System.Collections.Generic;
using Dapper;
using System.Linq;
using System.Windows.Forms;
using RoteiroOtimizado.DbContext;
using System.Net.Http;
using System.Text;
using System.Globalization;

namespace RoteiroOtimizado
{
    public partial class Form1 : Form
    {
        private List<RoteirosColeta> roteiros = null;
        private static readonly string tipoColeta = "DOMICILIAR";
        private string setor;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnGerar_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                ChamarApi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: "+ ex.Message);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private List<RoteirosColeta> ObterRoteiros()
        {
            List<RoteirosColeta> ret = null;
            var query = @"select *
                            from RoteirosColeta
                            where TipoColeta = @tipoColeta
                              and Setor = @setor
                              and OrdemTer is not null
                            order by OrdemTer";
            using (var connection = DbConnectionFactory.GetInstance())
            {
                ret = connection.Query<RoteirosColeta>(query, new { tipoColeta, setor }).ToList();
            }

            return ret;
        }

        /*
        private void ChamarApi2()
        {
            var culture = new CultureInfo("en-US");
            var app_id = txtAppId.Text;
            var app_code = txtAppCode.Text;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://route.cit.api.here.com");

                var request = new StringBuilder();

                request.Append(string.Format("routing/7.2/calculateroute.json" +
                            "?mode=fastest;truck;traffic:disabled;" +
                            "&departure=" +
                            "&representation=display" +
                            "&routeAttributes=all" +
                            "&app_id={0}" +
                            "&app_code={1}", app_id, app_code));

                var roteiros = ObterRoteiros();
                var ordem = 0;
                foreach (var item in roteiros)
                {
                    if (item == roteiros.First())
                    {
                        request.AppendFormat("&start=");
                        request.Append(string.Format("{0},{1}", item.Lat.ToString(culture), item.Lng.ToString(culture)));
                    }
                    else if (item == roteiros.Last())
                    {
                        request.AppendFormat("&end=");
                        request.Append(string.Format("{0},{1}", item.Lat.ToString(culture), item.Lng.ToString(culture)));
                    }
                    else
                    {
                        request.AppendFormat("&waypoint{0}=", ordem++);
                        request.Append(string.Format("{0},{1}", item.Lat.ToString(culture), item.Lng.ToString(culture)));
                    }
                }

                var apiResult = client.GetAsync(request.ToString()).Result;
                if (!apiResult.IsSuccessStatusCode) return;
                var result = apiResult.ConvertTo<Root>();
                //result.SaveJsonInFile();

                var list = result.response.route[0].waypoint;
                for (int i = 0; i < list.Length; i++)
                {
                    var item = list[i];

                    var roteiro = roteiros
                        .FirstOrDefault(x => Math.Round(item.originalPosition.latitude, 6) == x.Lat &&
                                              Math.Round(item.originalPosition.longitude, 6) == x.Lng);

                    if (roteiro == null)
                        continue;

                    roteiro.NovaOrdemTer = i + 2;
                }

                dataGridView1.DataSource = roteiros;
            }
        }
        */

        private void ChamarApi()
        {
            var culture = new CultureInfo("en-US");
            var app_id = txtAppId.Text;
            var app_code = txtAppCode.Text;
            setor = txtSetor.Text;

            if(string.IsNullOrEmpty(app_id))
            {
                MessageBox.Show("'APP ID' required");
                return;
            }

            if (string.IsNullOrEmpty(app_code))
            {
                MessageBox.Show("'APP CODE' required");
                return;
            }

            if (string.IsNullOrEmpty(setor))
            {
                MessageBox.Show("'SETOR' required");
                return;
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://wse.cit.api.here.com");

                var request = new StringBuilder();

                request.Append(string.Format("2/findsequence.json" +
                            "?mode=fastest;truck;traffic:disabled;" +
                            "&restTimes=disabled" +
                            "&app_id={0}" +
                            "&app_code={1}", app_id, app_code));

                roteiros = ObterRoteiros();
                var ordem = 1;
                foreach (var item in roteiros)
                {
                    if (item == roteiros.First())
                        request.AppendFormat("&start=start;");
                    else if (item == roteiros.Last())
                        request.AppendFormat("&end=end;");
                    else
                    {
                        request.AppendFormat("&destination{0}=ordem-{0};", ordem++);
                    }
                    request.Append(string.Format("{0},{1}", item.Lat.ToString(culture), item.Lng.ToString(culture)));
                }

                var apiResult = client.GetAsync(request.ToString()).Result;
                if (!apiResult.IsSuccessStatusCode) return;
                var result = apiResult.ConvertTo<Root>();
                //result.SaveJsonInFile();

                var list = result.results[0].waypoints;
                for (int i = 0; i < list.Length; i++)
                {
                    var item = list[i];

                    var roteiro = roteiros
                        .FirstOrDefault(x => Math.Round(item.lat, 6) == x.Lat &&
                                              Math.Round(item.lng, 6) == x.Lng);

                    if (roteiro == null)
                        continue;

                    roteiro.NovaOrdemTer = item.sequence + 1;
                }

                dataGridView1.DataSource = roteiros;
                btnUpdate.Enabled = true;
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNovoSetor.Text))
            {
                MessageBox.Show("'NOVO SETOR' required");
                return;
            }

            if (txtNovoSetor.Text == txtSetor.Text)
            {
                MessageBox.Show("'SETOR' e 'NOVO SETOR' precisam ser diferentes");
                return;
            }

            CreateRoteiro();

            MessageBox.Show("Finalizado");
        }

        private void CreateRoteiro()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                using (var connection = DbConnectionFactory.GetInstance())
                {
                    var queryDel = @"delete from RoteirosColeta where setor = @setor";
                    connection.Execute(queryDel, new { setor = txtNovoSetor.Text });

                    var query = @"insert into RoteirosColeta (CEPD,CEPE,Tipo,Titulo,Preposicao,Logradouro,Inicio,Fim,Distrito,SubPrefeitura,Frequencia,HorarioSeg,HorarioTer,HorarioQua,HorarioQui,HorarioSex,HorarioSab,HorarioDom,TipoColeta,Longitude,Latitude,Setor,NumeroIniD,NumeroFimD,NumeroIniE,NumeroFimE,DataAlteracao,IdMaptitude,TurnoOperacionalId,Lat,Lng,FormaColeta,OrdemSeg,OrdemTer,OrdemQua,OrdemQui,OrdemSex,OrdemSab,OrdemDom)
                             select CEPD,CEPE,Tipo,Titulo,Preposicao,Logradouro,Inicio,Fim,Distrito,SubPrefeitura,Frequencia,HorarioSeg,HorarioTer,HorarioQua,HorarioQui,HorarioSex,HorarioSab,HorarioDom,TipoColeta,Longitude,Latitude,@setor,NumeroIniD,NumeroFimD,NumeroIniE,NumeroFimE,DataAlteracao,IdMaptitude,TurnoOperacionalId,Lat,Lng,FormaColeta,
                                    case when OrdemSeg is null then OrdemSeg else @ordem end,
                                    case when OrdemTer is null then OrdemTer else @ordem end,
                                    case when OrdemQua is null then OrdemQua else @ordem end,
                                    case when OrdemQui is null then OrdemQui else @ordem end,
                                    case when OrdemSex is null then OrdemSex else @ordem end,
                                    case when OrdemSab is null then OrdemSab else @ordem end,
                                    case when OrdemDom is null then OrdemDom else @ordem end
                               from RoteirosColeta
                              where id = @Id";

                    foreach (var item in roteiros)
                    {
                        try
                        {
                            connection.Execute(query, new { item.Id, ordem = item.NovaOrdemTer, setor = txtNovoSetor.Text });
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void UpdateRoteiro()
        {
            using (var connection = DbConnectionFactory.GetInstance())
            {
                var query = @"update RoteirosColeta
                                set OrdemSeg = case when OrdemSeg is null then OrdemSeg else @ordem end,
                                    OrdemTer = case when OrdemTer is null then OrdemTer else @ordem end,
                                    OrdemQua = case when OrdemQua is null then OrdemQua else @ordem end,
                                    OrdemQui = case when OrdemQui is null then OrdemQui else @ordem end,
                                    OrdemSex = case when OrdemSex is null then OrdemSex else @ordem end,
                                    OrdemSab = case when OrdemSab is null then OrdemSab else @ordem end,
                                    OrdemDom = case when OrdemDom is null then OrdemDom else @ordem end
                                where id = @Id";

                foreach (var item in roteiros.Where(x => x.NovaOrdemTer != x.OrdemTer))
                {
                    try
                    {
                        connection.Execute(query, new { item.Id, ordem = item.NovaOrdemTer });
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        private void txtSetor_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void txtSetor_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSetor.Text) && string.IsNullOrEmpty(txtNovoSetor.Text))
            {
                txtNovoSetor.Text = txtSetor.Text + "-1";
            }
        }
    }
}
