namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip manifest is in its life cycle.</summary>
public enum PayslipManifestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A payslip manifest. Its code is unique within a tenant.</summary>
public sealed class PayslipManifest(string code, string title, double score, string source, string summary, double hours, string team, Dictionary<string, string> attributes, int startsAt, int updatedAt, PayslipManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public string Source { get; set; } = source;
    public string Summary { get; set; } = summary;
    public double Hours { get; set; } = hours;
    public string Team { get; set; } = team;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int StartsAt { get; set; } = startsAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public PayslipManifestStatus Status { get; set; } = status;

    /// <summary>Starts a payslip manifest in status requested.</summary>
    public static PayslipManifest NewPayslipManifest(string code, string title)
    {
        return new PayslipManifest(
            code,
            title,
            0.0,
            "",
            "",
            0.0,
            "",
            new Dictionary<string, string>(),
            0,
            0,
            PayslipManifestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipManifestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipManifestStatus next)
    {
        switch (Status)
        {
            case PayslipManifestStatus.Requested:
                switch (next)
                {
                    case PayslipManifestStatus.Revoked:
                    case PayslipManifestStatus.Expired:
                        return true;
                }
                return false;
            case PayslipManifestStatus.Granted:
                return next == PayslipManifestStatus.Revoked || next == PayslipManifestStatus.Expired;
            case PayslipManifestStatus.Revoked:
                return next == PayslipManifestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PayslipManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"payslip manifest"} {Code}";
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

    public double AdjustedValue()
    {
        return Math.Round((double)StartsAt * Hours, 2);
    }

    public PayslipManifest Copy()
    {
        return new PayslipManifest(
            Code,
            Title,
            Score,
            Source,
            Summary,
            Hours,
            Team,
            Attributes,
            StartsAt,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
