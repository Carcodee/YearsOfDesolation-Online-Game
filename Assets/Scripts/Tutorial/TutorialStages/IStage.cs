public interface IStage
{
    TutorialStage stage
    {
        get;
        set;
    }
     bool hasDialogFinished
     {
         get;
         set;
     }
     bool hasUIIndicationsFinished
     {
         get;
         set;
     }
     public void OnDialogDisplayed();
    public void OnDialogFinished();
    public void OnUIInstruction();
    public void OnStageGoing();
    public void OnStageEnded();
}
