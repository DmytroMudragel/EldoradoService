
namespace EldoradoBot.Responses
{
    public class ResumeAccsResponse
    {
        public class Root
        {
            public int offersInGivenFilterCount { get; set; }

            public int resumedOffersCount { get; set; }

            public bool totalOffersToResumeLimitReached { get; set; }
        }
    }
}
