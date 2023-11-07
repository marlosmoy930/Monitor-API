using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Scores.Models
{
    public class ScoreLabelModel
    {
        public int Min { get; set; }

        public int Max { get; set; }

        public string Label { get; set; }

        public string Color { get; set; }


        public void Update(ScoreSystemElement entity)
        {
            entity.Min = Min;
            entity.Max = Max;
            entity.Label = Label;
        }
    }
}