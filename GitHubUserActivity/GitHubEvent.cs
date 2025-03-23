namespace GitHubUserActivity
{
    class GitHubEvent
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public GitHubActor Actor { get; set; }
    }
}
