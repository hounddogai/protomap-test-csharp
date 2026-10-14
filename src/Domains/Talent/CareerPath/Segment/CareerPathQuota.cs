namespace Acme.Workforce.Talent.CareerPath.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path quota is in its life cycle.</summary>
public enum CareerPathQuotaStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A career path quota. Its code is unique within a tenant.</summary>
public sealed class CareerPathQuota(string code, string title, Dictionary<string, string> attributes, double score, string costCenter, string reviewer, string locale, CareerPathQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Score { get; set; } = score;
    public string CostCenter { get; set; } = costCenter;
    public string Reviewer { get; set; } = reviewer;
    public string Locale { get; set; } = locale;
    public CareerPathQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a career path quota in status planned.</summary>
    public static CareerPathQuota NewCareerPathQuota(string code, string title)
    {
        return new CareerPathQuota(
            code,
            title,
            new Dictionary<string, string>(),
            0.0,
            "",
            "",
            "",
            CareerPathQuotaStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathQuotaStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathQuotaStatus next)
    {
        switch (Status)
        {
            case CareerPathQuotaStatus.Planned:
                return next == CareerPathQuotaStatus.Completed;
            case CareerPathQuotaStatus.Scheduled:
                return next == CareerPathQuotaStatus.Completed || next == CareerPathQuotaStatus.Cancelled;
            case CareerPathQuotaStatus.InProgress:
                switch (next)
                {
                    case CareerPathQuotaStatus.Completed:
                    case CareerPathQuotaStatus.Cancelled:
                        return true;
                }
                return false;
            case CareerPathQuotaStatus.Completed:
                return next == CareerPathQuotaStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CareerPathQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double MeasuredValue()
    {
        return Score;
    }

    public CareerPathQuota Copy()
    {
        return new CareerPathQuota(Code, Title, Attributes, Score, CostCenter, Reviewer, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
