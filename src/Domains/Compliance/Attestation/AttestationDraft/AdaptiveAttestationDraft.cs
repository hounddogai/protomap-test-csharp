namespace Acme.Workforce.Compliance.Attestation.AttestationDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive attestation draft is in its life cycle.</summary>
public enum AdaptiveAttestationDraftStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A adaptive attestation draft. Its code is unique within a tenant.</summary>
public sealed class AdaptiveAttestationDraft(string code, string name, int batchSize, string employeeId, string jobCode, string phone, string timezone, bool active, int leaveDays, string salaryBand, string currency, AdaptiveAttestationDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public string EmployeeId { get; set; } = employeeId;
    public string JobCode { get; set; } = jobCode;
    public string Phone { get; set; } = phone;
    public string Timezone { get; set; } = timezone;
    public bool Active { get; set; } = active;
    public int LeaveDays { get; set; } = leaveDays;
    public string SalaryBand { get; set; } = salaryBand;
    public string Currency { get; set; } = currency;
    public AdaptiveAttestationDraftStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive attestation draft in status draft.</summary>
    public static AdaptiveAttestationDraft NewAdaptiveAttestationDraft(string code, string name)
    {
        return new AdaptiveAttestationDraft(
            code,
            name,
            0,
            "",
            "",
            "",
            "",
            false,
            0,
            "",
            "",
            AdaptiveAttestationDraftStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveAttestationDraftStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveAttestationDraftStatus next)
    {
        switch (Status)
        {
            case AdaptiveAttestationDraftStatus.Draft:
                switch (next)
                {
                    case AdaptiveAttestationDraftStatus.Pending:
                        return true;
                }
                return false;
            case AdaptiveAttestationDraftStatus.Pending:
                switch (next)
                {
                    case AdaptiveAttestationDraftStatus.Suspended:
                        return true;
                }
                return false;
            case AdaptiveAttestationDraftStatus.Active:
                switch (next)
                {
                    case AdaptiveAttestationDraftStatus.Closed:
                        return true;
                }
                return false;
            case AdaptiveAttestationDraftStatus.Suspended:
                switch (next)
                {
                    case AdaptiveAttestationDraftStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveAttestationDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive attestation draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)LeaveDays * (double)BatchSize, 2);
    }

    public AdaptiveAttestationDraft Copy()
    {
        return new AdaptiveAttestationDraft(
            Code,
            Name,
            BatchSize,
            EmployeeId,
            JobCode,
            Phone,
            Timezone,
            Active,
            LeaveDays,
            SalaryBand,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
