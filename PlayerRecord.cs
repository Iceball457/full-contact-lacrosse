public record PlayerRecord {
    public enum Role {
        Thrower,
        Runner,
        Tackler
    }
    public readonly int deviceIdx;
    public readonly bool team1;
    public readonly Role role;

    public PlayerRecord(int deviceIdx, bool team1, Role role) {
        this.deviceIdx = deviceIdx;
        this.team1 = team1;
        this.role = role;
    }
}