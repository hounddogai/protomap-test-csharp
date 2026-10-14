namespace Acme.Workforce.Talent.SkillProfile.Policy;

using Acme.Workforce.Talent.CareerPath.Agreement;

/// <summary>Functions of skill profile forecast from agreement.</summary>
public static class SkillProfileForecastFromAgreement
{
    public static Dictionary<string, double> SummarizeCareerPathAgreementsForForecast(List<CareerPathAgreement> items)
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

    public static List<SkillProfileForecast> DeriveSkillProfileForecastsFromAgreement(List<CareerPathAgreement> items)
    {
        var outValue = new List<SkillProfileForecast>();
        foreach (var item in items)
        {
            var derived = SkillProfileForecast.NewSkillProfileForecast(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateSkillProfileForecast(SkillProfileForecast item)
    {
        return CareerPathManifestPolicy.ClassifyCareerPathManifest(item.EffectiveValue());
    }
}
