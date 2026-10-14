namespace Acme.Workforce.Talent.CareerPath.Exception;

using Acme.Workforce.Talent.CareerPath.Segment;

/// <summary>Functions of career path notice from segment.</summary>
public static class CareerPathNoticeFromSegment
{
    public static Dictionary<string, double> SummarizeCareerPathSegmentsForNotice(List<CareerPathSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<CareerPathNotice> DeriveCareerPathNoticesFromSegment(List<CareerPathSegment> items)
    {
        var outValue = new List<CareerPathNotice>();
        foreach (var item in items)
        {
            var derived = CareerPathNotice.NewCareerPathNotice(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignCareerPathNotice(CareerPathNotice item)
    {
        return CareerPathSegmentPolicy.NormalizeCareerPathSegmentCode(item.Code);
    }
}
