namespace RoteiroOtimizado
{
    public class RoteirosColeta : BaseClass
    {
        public string HorarioTer { get; set; }
        public int? OrdemTer { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }

        public int? NovaOrdemTer { get; set; }
    }
}
