namespace Acme.Workforce.Workplace.Desk.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk review.</summary>
public sealed record DeskReviewView(string CodeText, string Name, string ManagerId, string Description, string Reviewer, string EmployeeId, string SkillText, string CostText, string StatusLabel)
{
    public static DeskReviewView ToDeskReviewView(DeskReview item)
    {
        return new DeskReviewView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.ManagerId,
            Text.Truncate(item.Description, 40),
            Text.Truncate(item.Reviewer, 80),
            item.EmployeeId,
            Text.Truncate(item.Skill, 64),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskReviewView> ToDeskReviewViews(List<DeskReview> items)
    {
        var views = new List<DeskReviewView>();
        foreach (var item in items)
        {
            views.Add(ToDeskReviewView(item));
        }
        return views;
    }

    public static double SumDeskReviewView(DeskReviewView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return parsedCost;
    }
}
