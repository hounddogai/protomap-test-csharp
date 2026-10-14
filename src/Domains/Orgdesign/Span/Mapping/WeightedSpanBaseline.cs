namespace Acme.Workforce.Orgdesign.Span.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted span baseline is in its life cycle.</summary>
public enum WeightedSpanBaselineStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A weighted span baseline. Its code is unique within a tenant.</summary>
public sealed class WeightedSpanBaseline(string code, string title, double floorValue, string salaryBand, int endsAt, string ownerId, string skill, string phone, WeightedSpanBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public string SalaryBand { get; set; } = salaryBand;
    public int EndsAt { get; set; } = endsAt;
    public string OwnerId { get; set; } = ownerId;
    public string Skill { get; set; } = skill;
    public string Phone { get; set; } = phone;
    public WeightedSpanBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a weighted span baseline in status idle.</summary>
    public static WeightedSpanBaseline NewWeightedSpanBaseline(string code, string title)
    {
        return new WeightedSpanBaseline(code, title, 0.0, "", 0, "", "", "", WeightedSpanBaselineStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedSpanBaselineStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedSpanBaselineStatus next)
    {
        switch (Status)
        {
            case WeightedSpanBaselineStatus.Idle:
                return next == WeightedSpanBaselineStatus.Ready;
            case WeightedSpanBaselineStatus.Warming:
                return next == WeightedSpanBaselineStatus.Ready || next == WeightedSpanBaselineStatus.Stopped;
            case WeightedSpanBaselineStatus.Ready:
                return next == WeightedSpanBaselineStatus.Draining;
            case WeightedSpanBaselineStatus.Draining:
                switch (next)
                {
                    case WeightedSpanBaselineStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedSpanBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted span baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"weighted span baseline"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)EndsAt * FloorValue, 2);
    }

    public WeightedSpanBaseline Copy()
    {
        return new WeightedSpanBaseline(Code, Title, FloorValue, SalaryBand, EndsAt, OwnerId, Skill, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
