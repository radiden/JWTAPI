namespace jwtapi.Models
{
    public class Refresh
    {
        public int Id { get; set; }
        public string JWT { get; set; }
        public string RefreshToken { get; set; }
    }
}
