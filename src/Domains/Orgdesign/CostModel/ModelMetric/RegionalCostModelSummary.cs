namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional cost model summary is in its life cycle.</summary>
public enum RegionalCostModelSummaryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A regional cost model summary as the cost model team keeps it.</summary>
public sealed class RegionalCostModelSummary(string code, string title, double rating, string category, int createdAt, string currency, string costCenter, RegionalCostModelSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public string Category { get; set; } = category;
    public int CreatedAt { get; set; } = createdAt;
    public string Currency { get; set; } = currency;
    public string CostCenter { get; set; } = costCenter;
    public RegionalCostModelSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a regional cost model summary in status created.</summary>
    public static RegionalCostModelSummary NewRegionalCostModelSummary(string code, string title)
    {
        return new RegionalCostModelSummary(code, title, 0.0, "", 0, "", "", RegionalCostModelSummaryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalCostModelSummaryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalCostModelSummaryStatus next)
    {
        switch (Status)
        {
            case RegionalCostModelSummaryStatus.Created:
                return next == RegionalCostModelSummaryStatus.Retired;
            case RegionalCostModelSummaryStatus.Validated:
                switch (next)
                {
                    case RegionalCostModelSummaryStatus.Retired:
                        return true;
                }
                return false;
            case RegionalCostModelSummaryStatus.Published:
                switch (next)
                {
                    case RegionalCostModelSummaryStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalCostModelSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional cost model summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Rating + (double)CreatedAt, 2);
    }

    public RegionalCostModelSummary Copy()
    {
        return new RegionalCostModelSummary(Code, Title, Rating, Category, CreatedAt, Currency, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
