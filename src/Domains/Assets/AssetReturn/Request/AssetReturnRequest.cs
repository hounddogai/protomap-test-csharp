namespace Acme.Workforce.Assets.AssetReturn.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return request is in its life cycle.</summary>
public enum AssetReturnRequestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A asset return request. Its code is unique within a tenant.</summary>
public sealed class AssetReturnRequest(string code, string title, string contactEmail, string skill, string externalRef, string department, string label, double amount, AssetReturnRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public string Skill { get; set; } = skill;
    public string ExternalRef { get; set; } = externalRef;
    public string Department { get; set; } = department;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public AssetReturnRequestStatus Status { get; set; } = status;

    /// <summary>Starts a asset return request in status requested.</summary>
    public static AssetReturnRequest NewAssetReturnRequest(string code, string title)
    {
        return new AssetReturnRequest(code, title, "", "", "", "", "", 0.0, AssetReturnRequestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnRequestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnRequestStatus next)
    {
        switch (Status)
        {
            case AssetReturnRequestStatus.Requested:
                return next == AssetReturnRequestStatus.Revoked || next == AssetReturnRequestStatus.Expired;
            case AssetReturnRequestStatus.Granted:
                return next == AssetReturnRequestStatus.Expired;
            case AssetReturnRequestStatus.Revoked:
                return next == AssetReturnRequestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AssetReturnRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public AssetReturnRequest Copy()
    {
        return new AssetReturnRequest(
            Code,
            Title,
            ContactEmail,
            Skill,
            ExternalRef,
            Department,
            Label,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
