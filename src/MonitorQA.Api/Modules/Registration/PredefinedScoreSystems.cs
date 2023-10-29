using System.Collections.Generic;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Registration
{
    public static class PredefinedScoreSystems
    {
        public static ScoreSystem[] GetScoreSystems(Company company)
        {
            return new[]
            {
                new ScoreSystem
                {
                    Company = company,
                    Name = "Default Scoring",
                    ScoreSystemElements = new List<ScoreSystemElement>
                    {
                        new ScoreSystemElement
                        {
                            Label = "Failed",
                            Min = 0,
                            Max = 69,
                            Color = "#FF4D4F"

                        },
                        new ScoreSystemElement
                        {
                            Label = "Passed",
                            Min = 70,
                            Max = 100,
                            Color = "#00C8A8"
                        }
                    },
                    PassedThreshold = 70
                }
            };
        }
    }
}
