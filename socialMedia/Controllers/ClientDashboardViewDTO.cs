namespace socialMedia.Controllers
{
    internal class ClientDashboardViewDTO
    {
        public object TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int PendingTasks { get; set; }
        public object RecentTasks { get; set; }
    }
}