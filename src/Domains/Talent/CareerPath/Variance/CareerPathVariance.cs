namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path variance is in its life cycle.</summary>
public enum CareerPathVarianceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A career path variance as the career path team keeps it.</summary>
public sealed class CareerPathVariance(string code, string title, int attempts, string location, string category, double budget, Dictionary<string, string> attributes, int windowDays, string timezone, CareerPathVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public string Location { get; set; } = location;
    public string Category { get; set; } = category;
    public double Budget { get; set; } = budget;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int WindowDays { get; set; } = windowDays;
    public string Timezone { get; set; } = timezone;
    public CareerPathVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a career path variance in status open.</summary>
    public static CareerPathVariance NewCareerPathVariance(string code, string title)
    {
        return new CareerPathVariance(
            code,
            title,
            0,
            "",
            "",
            0.0,
            new Dictionary<string, string>(),
            0,
            "",
            CareerPathVarianceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathVarianceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathVarianceStatus next)
    {
        switch (Status)
        {
            case CareerPathVarianceStatus.Open:
                return next == CareerPathVarianceStatus.Resolved;
            case CareerPathVarianceStatus.InReview:
                return next == CareerPathVarianceStatus.Resolved || next == CareerPathVarianceStatus.Archived;
            case CareerPathVarianceStatus.Resolved:
                return next == CareerPathVarianceStatus.Archived;
            case CareerPathVarianceStatus.Reopened:
                return next == CareerPathVarianceStatus.Archived;
        }
        return false;
    }

    public void MoveTo(CareerPathVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetTotal()
    {
        return Math.Round((double)WindowDays + Budget, 2);
    }

    public CareerPathVariance Copy()
    {
        return new CareerPathVariance(
            Code,
            Title,
            Attempts,
            Location,
            Category,
            Budget,
            Attributes,
            WindowDays,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
