namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional training due request is in its life cycle.</summary>
public enum RegionalTrainingDueRequestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A regional training due request. Its code is unique within a tenant.</summary>
public sealed class RegionalTrainingDueRequest(string code, string label, double weight, double cost, double discount, string managerId, RegionalTrainingDueRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public double Cost { get; set; } = cost;
    public double Discount { get; set; } = discount;
    public string ManagerId { get; set; } = managerId;
    public RegionalTrainingDueRequestStatus Status { get; set; } = status;

    /// <summary>Starts a regional training due request in status created.</summary>
    public static RegionalTrainingDueRequest NewRegionalTrainingDueRequest(string code, string label)
    {
        return new RegionalTrainingDueRequest(
            code,
            label,
            0.0,
            0.0,
            0.0,
            "",
            RegionalTrainingDueRequestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalTrainingDueRequestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalTrainingDueRequestStatus next)
    {
        switch (Status)
        {
            case RegionalTrainingDueRequestStatus.Created:
                return next == RegionalTrainingDueRequestStatus.Retired;
            case RegionalTrainingDueRequestStatus.Validated:
                switch (next)
                {
                    case RegionalTrainingDueRequestStatus.Retired:
                        return true;
                }
                return false;
            case RegionalTrainingDueRequestStatus.Published:
                switch (next)
                {
                    case RegionalTrainingDueRequestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalTrainingDueRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional training due request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Discount + Weight, 2);
    }

    public RegionalTrainingDueRequest Copy()
    {
        return new RegionalTrainingDueRequest(Code, Label, Weight, Cost, Discount, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
