namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay variance is in its life cycle.</summary>
public enum RetroPayVarianceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A retro pay variance. Its code is unique within a tenant.</summary>
public sealed class RetroPayVariance(string code, string name, int tenureMonths, bool archived, Dictionary<string, string> attributes, RetroPayVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Archived { get; set; } = archived;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public RetroPayVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay variance in status proposed.</summary>
    public static RetroPayVariance NewRetroPayVariance(string code, string name)
    {
        return new RetroPayVariance(
            code,
            name,
            0,
            false,
            new Dictionary<string, string>(),
            RetroPayVarianceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayVarianceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayVarianceStatus next)
    {
        switch (Status)
        {
            case RetroPayVarianceStatus.Proposed:
                switch (next)
                {
                    case RetroPayVarianceStatus.Rejected:
                    case RetroPayVarianceStatus.Withdrawn:
                        return true;
                }
                return false;
            case RetroPayVarianceStatus.Approved:
                return next == RetroPayVarianceStatus.Withdrawn;
            case RetroPayVarianceStatus.Rejected:
                return next == RetroPayVarianceStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RetroPayVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"retro pay variance"} {Code}";
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
        return (double)TenureMonths;
    }

    public RetroPayVariance Copy()
    {
        return new RetroPayVariance(Code, Name, TenureMonths, Archived, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
