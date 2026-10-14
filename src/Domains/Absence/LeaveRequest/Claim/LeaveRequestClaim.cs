namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request claim is in its life cycle.</summary>
public enum LeaveRequestClaimStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A leave request claim as the leave request team keeps it.</summary>
public sealed class LeaveRequestClaim(string code, string title, string currency, string contactEmail, string salaryBand, string description, List<string> tags, string phone, int updatedAt, int expiresAt, double discount, LeaveRequestClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public string ContactEmail { get; set; } = contactEmail;
    public string SalaryBand { get; set; } = salaryBand;
    public string Description { get; set; } = description;
    public List<string> Tags { get; set; } = tags;
    public string Phone { get; set; } = phone;
    public int UpdatedAt { get; set; } = updatedAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Discount { get; set; } = discount;
    public LeaveRequestClaimStatus Status { get; set; } = status;

    /// <summary>Starts a leave request claim in status created.</summary>
    public static LeaveRequestClaim NewLeaveRequestClaim(string code, string title)
    {
        return new LeaveRequestClaim(
            code,
            title,
            "",
            "",
            "",
            "",
            new List<string>(),
            "",
            0,
            0,
            0.0,
            LeaveRequestClaimStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestClaimStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestClaimStatus next)
    {
        switch (Status)
        {
            case LeaveRequestClaimStatus.Created:
                switch (next)
                {
                    case LeaveRequestClaimStatus.Validated:
                    case LeaveRequestClaimStatus.Published:
                        return true;
                }
                return false;
            case LeaveRequestClaimStatus.Validated:
                switch (next)
                {
                    case LeaveRequestClaimStatus.Retired:
                        return true;
                }
                return false;
            case LeaveRequestClaimStatus.Published:
                switch (next)
                {
                    case LeaveRequestClaimStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"leave request claim"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetExposure()
    {
        return Math.Round(Discount - (double)ExpiresAt, 2);
    }

    public LeaveRequestClaim Copy()
    {
        return new LeaveRequestClaim(
            Code,
            Title,
            Currency,
            ContactEmail,
            SalaryBand,
            Description,
            Tags,
            Phone,
            UpdatedAt,
            ExpiresAt,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
