namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted merit cycle tier is in its life cycle.</summary>
public enum WeightedMeritCycleTierStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A weighted merit cycle tier as the merit cycle team keeps it.</summary>
public sealed class WeightedMeritCycleTier(string code, string label, double maximum, string contactEmail, double floorValue, WeightedMeritCycleTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public string ContactEmail { get; set; } = contactEmail;
    public double FloorValue { get; set; } = floorValue;
    public WeightedMeritCycleTierStatus Status { get; set; } = status;

    /// <summary>Starts a weighted merit cycle tier in status idle.</summary>
    public static WeightedMeritCycleTier NewWeightedMeritCycleTier(string code, string label)
    {
        return new WeightedMeritCycleTier(code, label, 0.0, "", 0.0, WeightedMeritCycleTierStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedMeritCycleTierStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedMeritCycleTierStatus next)
    {
        switch (Status)
        {
            case WeightedMeritCycleTierStatus.Idle:
                return next == WeightedMeritCycleTierStatus.Ready || next == WeightedMeritCycleTierStatus.Stopped;
            case WeightedMeritCycleTierStatus.Warming:
                switch (next)
                {
                    case WeightedMeritCycleTierStatus.Ready:
                    case WeightedMeritCycleTierStatus.Stopped:
                        return true;
                }
                return false;
            case WeightedMeritCycleTierStatus.Ready:
                return next == WeightedMeritCycleTierStatus.Draining || next == WeightedMeritCycleTierStatus.Stopped;
            case WeightedMeritCycleTierStatus.Draining:
                switch (next)
                {
                    case WeightedMeritCycleTierStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedMeritCycleTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted merit cycle tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"weighted merit cycle tier"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Maximum - FloorValue, 2);
    }

    public WeightedMeritCycleTier Copy()
    {
        return new WeightedMeritCycleTier(Code, Label, Maximum, ContactEmail, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
