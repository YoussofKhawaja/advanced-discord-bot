namespace ADB.Models
{
    public class Food
    {
        public string image;
    }
    public class MALUser
    {
        public string username;
        public string image_url;
        public AnimeList anime_stats;
    }
    public class AnimeList
    {
        public double days_watched;
        public double mean_score;
        public int watching;
        public int completed;
        public int dropped;
        public int total_entries;
    }

    public class Anime
    {
        public AnimeResults[] results;
    }
    public class AnimeResults
    {
        public string image_url;
        public string title;
        public bool airing;
        public string synopsis;
        public string type;
        public double score;
        public int episodes;
        public string rated;
        public int members;
    }

}
