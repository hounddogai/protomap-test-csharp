namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional engagement budget is in its life cycle.</summary>
public enum RegionalEngagementBudgetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a regional engagement budget and its current status.</summary>
public sealed class RegionalEngagementBudget(string code, string label, double margin, string salaryBand, double weight, List<string> tags, double rating, int expiresAt, double floorValue, string locale, RegionalEngagementBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public string SalaryBand { get; set; } = salaryBand;
    public double Weight { get; set; } = weight;
    public List<string> Tags { get; set; } = tags;
    public double Rating { get; set; } = rating;
    public int ExpiresAt { get; set; } = expiresAt;
    public double FloorValue { get; set; } = floorValue;
    public string Locale { get; set; } = locale;
    public RegionalEngagementBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a regional engagement budget in status requested.</summary>
    public static RegionalEngagementBudget NewRegionalEngagementBudget(string code, string label)
    {
        return new RegionalEngagementBudget(
            code,
            label,
            0.0,
            "",
            0.0,
            new List<string>(),
            0.0,
            0,
            0.0,
            "",
            RegionalEngagementBudgetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalEngagementBudgetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalEngagementBudgetStatus next)
    {
        switch (Status)
        {
            case RegionalEngagementBudgetStatus.Requested:
                switch (next)
                {
                    case RegionalEngagementBudgetStatus.Granted:
                    case RegionalEngagementBudgetStatus.Expired:
                        return true;
                }
                return false;
            case RegionalEngagementBudgetStatus.Granted:
                return next == RegionalEngagementBudgetStatus.Expired;
            case RegionalEngagementBudgetStatus.Revoked:
                return next == RegionalEngagementBudgetStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RegionalEngagementBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional engagement budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetTotal()
    {
        return Math.Round((double)ExpiresAt - FloorValue, 2);
    }

    public RegionalEngagementBudget Copy()
    {
        return new RegionalEngagementBudget(
            Code,
            Label,
            Margin,
            SalaryBand,
            Weight,
            Tags,
            Rating,
            ExpiresAt,
            FloorValue,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
