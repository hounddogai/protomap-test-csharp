namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band bundle is in its life cycle.</summary>
public enum PayBandBundleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a pay band bundle and its current status.</summary>
public sealed class PayBandBundle(string code, string name, int attempts, string employeeId, int endsAt, double weight, PayBandBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string EmployeeId { get; set; } = employeeId;
    public int EndsAt { get; set; } = endsAt;
    public double Weight { get; set; } = weight;
    public PayBandBundleStatus Status { get; set; } = status;

    /// <summary>Starts a pay band bundle in status idle.</summary>
    public static PayBandBundle NewPayBandBundle(string code, string name)
    {
        return new PayBandBundle(code, name, 0, "", 0, 0.0, PayBandBundleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandBundleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandBundleStatus next)
    {
        switch (Status)
        {
            case PayBandBundleStatus.Idle:
                switch (next)
                {
                    case PayBandBundleStatus.Ready:
                    case PayBandBundleStatus.Stopped:
                        return true;
                }
                return false;
            case PayBandBundleStatus.Warming:
                return next == PayBandBundleStatus.Ready;
            case PayBandBundleStatus.Ready:
                return next == PayBandBundleStatus.Draining || next == PayBandBundleStatus.Stopped;
            case PayBandBundleStatus.Draining:
                switch (next)
                {
                    case PayBandBundleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Weight - (double)EndsAt, 2);
    }

    public PayBandBundle Copy()
    {
        return new PayBandBundle(Code, Name, Attempts, EmployeeId, EndsAt, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
