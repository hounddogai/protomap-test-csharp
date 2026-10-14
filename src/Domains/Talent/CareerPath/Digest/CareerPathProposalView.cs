namespace Acme.Workforce.Talent.CareerPath.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path proposal.</summary>
public sealed record CareerPathProposalView(string CodeText, string NameText, string PhoneText, string Fte, string RatioText, string Sequence, string MaximumText, string Currency, string GraceDays, string Amount, string StatusLabel)
{
    public static CareerPathProposalView ToCareerPathProposalView(CareerPathProposal item)
    {
        return new CareerPathProposalView(
            Text.Truncate(item.Code, 40),
            item.Name,
            Text.Truncate(item.Phone, 40),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 64),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathProposalView> ToCareerPathProposalViews(List<CareerPathProposal> items)
    {
        var views = new List<CareerPathProposalView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathProposalView(item));
        }
        return views;
    }

    public static double SumCareerPathProposalView(CareerPathProposalView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return parsedFte + parsedRatio + (double)parsedSequence;
    }
}
