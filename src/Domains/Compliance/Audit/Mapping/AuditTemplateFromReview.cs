namespace Acme.Workforce.Compliance.Audit.Mapping;

using Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>Functions of audit template from review.</summary>
public static class AuditTemplateFromReview
{
    public static Dictionary<string, double> SummarizeTrainingDueReviewsForTemplate(List<TrainingDueReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<AuditTemplate> DeriveAuditTemplatesFromReview(List<TrainingDueReview> items)
    {
        var outValue = new List<AuditTemplate>();
        foreach (var item in items)
        {
            var derived = AuditTemplate.NewAuditTemplate(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeAuditTemplate(AuditTemplate item)
    {
        return TrainingDueProfilePolicy.TrainingDueProfileWeight(4, true);
    }
}
