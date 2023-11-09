using System.Collections.Generic;

namespace MonitorQA.Api.Modules.Scores.Models
{
    public class ScoreSystemModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal PassedThreshold { get; set; }

        public List<ScoreLabelModel> Labels { get; set; }
    }
}