namespace Acme.Workforce.Payrollops.Garnishment.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment digest is in its life cycle.</summary>
public enum GarnishmentDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a garnishment digest and its current status.</summary>
public sealed class GarnishmentDigest(string code, string label, int createdAt, int expiresAt, double weight, string location, bool active, GarnishmentDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Weight { get; set; } = weight;
    public string Location { get; set; } = location;
    public bool Active { get; set; } = active;
    public GarnishmentDigestStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment digest in status idle.</summary>
    public static GarnishmentDigest NewGarnishmentDigest(string code, string label)
    {
        return new GarnishmentDigest(code, label, 0, 0, 0.0, "", false, GarnishmentDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentDigestStatus next)
    {
        switch (Status)
        {
            case GarnishmentDigestStatus.Idle:
                return next == GarnishmentDigestStatus.Warming || next == GarnishmentDigestStatus.Draining;
            case GarnishmentDigestStatus.Warming:
                switch (next)
                {
                    case GarnishmentDigestStatus.Ready:
                    case GarnishmentDigestStatus.Stopped:
                        return true;
                }
                return false;
            case GarnishmentDigestStatus.Ready:
                switch (next)
                {
                    case GarnishmentDigestStatus.Draining:
                        return true;
                }
                return false;
            case GarnishmentDigestStatus.Draining:
                switch (next)
                {
                    case GarnishmentDigestStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Weight - (double)CreatedAt, 2);
    }

    public GarnishmentDigest Copy()
    {
        return new GarnishmentDigest(Code, Label, CreatedAt, ExpiresAt, Weight, Location, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
