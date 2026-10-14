namespace Acme.Workforce.Compliance.PolicyAck.AckReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched policy ack reservation is in its life cycle.</summary>
public enum BatchedPolicyAckReservationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A batched policy ack reservation. Its code is unique within a tenant.</summary>
public sealed class BatchedPolicyAckReservation(string code, string label, double minimum, string reviewer, Dictionary<string, string> attributes, string location, double budget, string summary, BatchedPolicyAckReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public string Reviewer { get; set; } = reviewer;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Location { get; set; } = location;
    public double Budget { get; set; } = budget;
    public string Summary { get; set; } = summary;
    public BatchedPolicyAckReservationStatus Status { get; set; } = status;

    /// <summary>Starts a batched policy ack reservation in status created.</summary>
    public static BatchedPolicyAckReservation NewBatchedPolicyAckReservation(string code, string label)
    {
        return new BatchedPolicyAckReservation(
            code,
            label,
            0.0,
            "",
            new Dictionary<string, string>(),
            "",
            0.0,
            "",
            BatchedPolicyAckReservationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedPolicyAckReservationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedPolicyAckReservationStatus next)
    {
        switch (Status)
        {
            case BatchedPolicyAckReservationStatus.Created:
                return next == BatchedPolicyAckReservationStatus.Retired;
            case BatchedPolicyAckReservationStatus.Validated:
                return next == BatchedPolicyAckReservationStatus.Published || next == BatchedPolicyAckReservationStatus.Retired;
            case BatchedPolicyAckReservationStatus.Published:
                switch (next)
                {
                    case BatchedPolicyAckReservationStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedPolicyAckReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched policy ack reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double AdjustedVolume()
    {
        return Math.Round(Minimum * Budget, 2);
    }

    public BatchedPolicyAckReservation Copy()
    {
        return new BatchedPolicyAckReservation(
            Code,
            Label,
            Minimum,
            Reviewer,
            Attributes,
            Location,
            Budget,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
