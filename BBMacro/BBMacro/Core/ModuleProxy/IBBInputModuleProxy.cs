public interface IBBInputModuleProxy
{
    void ProcessInput(BBInputSnapshot snapshot);

    BBInputSnapshot TakeSnapshot();

    void OnRecordStart();

    void OnRecordStop();

    void Clear();
}