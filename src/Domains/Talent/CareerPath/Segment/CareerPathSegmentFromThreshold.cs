namespace Acme.Workforce.Talent.CareerPath.Segment;

using Acme.Workforce.Talent.SkillProfile.Agreement;

/// <summary>Functions of career path segment from threshold.</summary>
public static class CareerPathSegmentFromThreshold
{
    public static Dictionary<string, double> SummarizeAdaptiveSkillProfileThresholdsForSegment(List<AdaptiveSkillProfileThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<CareerPathSegment> DeriveCareerPathSegmentsFromThreshold(List<AdaptiveSkillProfileThreshold> items)
    {
        var outValue = new List<CareerPathSegment>();
        foreach (var item in items)
        {
            var derived = CareerPathSegment.NewCareerPathSegment(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreCareerPathSegment(CareerPathSegment item)
    {
        return SkillProfileAgreementPolicy.ClassifySkillProfileAgreement(item.EffectiveTotal());
    }
}
