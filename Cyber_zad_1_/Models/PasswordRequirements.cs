namespace Cyber_zad_1_.Models
{
    public class PasswordRequirements
    {
        public PasswordRequirements()
        {
            this.Id = 0;
            this.MinimumLength = 8;
            this.RequireDigit = 1;
            this.RequireUppercase = 1;
            this.RequireSpecialChar=1;
            this.days = 30;

        }
        public int Id { get; set; }
        public int MinimumLength { get; set; }
        public int RequireUppercase { get; set; }
        public int RequireDigit { get; set; }
        public int RequireSpecialChar { get; set; }
        public int days { get; set; }
    }
}
