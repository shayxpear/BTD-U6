using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class Note : MonoBehaviour
{
    public SIDE side;
    public NoteState noteState;
    public enum SIDE { LEFT_SIDE, RIGHT_SIDE };
    public enum NoteState { Active, NotActive, Miss, Hit }

    public RectTransform noteTransform;

    private void Start()
    {
        noteTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        switch (noteState)
        {
            case NoteState.Active:
                gameObject.SetActive(true);
                break;
            case NoteState.NotActive:
                gameObject.SetActive(false);
                break;
            case NoteState.Miss:
                break;
            case NoteState.Hit:
                break;
        }
    }

    public IEnumerator NoteAnimation(Note note, Animator animator) //Plays animation before dequeuing note
    {
        while (true)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Miss") || stateInfo.IsName("Hit"))
            {
                if (stateInfo.IsName("Miss"))
                    noteState = NoteState.Miss;
                else if (stateInfo.IsName("Hit"))
                    noteState = NoteState.Hit;

                yield return new WaitForSeconds(stateInfo.length);
                break;
            }
            yield return null;
        }

        note.gameObject.SetActive(false);
    }
}
