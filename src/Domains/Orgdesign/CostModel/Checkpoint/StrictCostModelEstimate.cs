namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict cost model estimate is in its life cycle.</summary>
public enum StrictCostModelEstimateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a strict cost model estimate and its current status.</summary>
public sealed class StrictCostModelEstimate(string code, string title, int version, int limit, double rate, string phone, bool pinned, StrictCostModelEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public int Limit { get; set; } = limit;
    public double Rate { get; set; } = rate;
    public string Phone { get; set; } = phone;
    public bool Pinned { get; set; } = pinned;
    public StrictCostModelEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a strict cost model estimate in status idle.</summary>
    public static StrictCostModelEstimate NewStrictCostModelEstimate(string code, string title)
    {
        return new StrictCostModelEstimate(code, title, 0, 0, 0.0, "", false, StrictCostModelEstimateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictCostModelEstimateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictCostModelEstimateStatus next)
    {
        switch (Status)
        {
            case StrictCostModelEstimateStatus.Idle:
                switch (next)
                {
                    case StrictCostModelEstimateStatus.Warming:
                        return true;
                }
                return false;
            case StrictCostModelEstimateStatus.Warming:
                switch (next)
                {
                    case StrictCostModelEstimateStatus.Draining:
                    case StrictCostModelEstimateStatus.Stopped:
                        return true;
                }
                return false;
            case StrictCostModelEstimateStatus.Ready:
                switch (next)
                {
                    case StrictCostModelEstimateStatus.Draining:
                        return true;
                }
                return false;
            case StrictCostModelEstimateStatus.Draining:
                switch (next)
                {
                    case StrictCostModelEstimateStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictCostModelEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict cost model estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Version - (double)Limit, 2);
    }

    public StrictCostModelEstimate Copy()
    {
        return new StrictCostModelEstimate(Code, Title, Version, Limit, Rate, Phone, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
