using System.Collections;
using TMPro;
using UnityEngine;

public sealed class LoadingDotsAnimator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _dotsText;
    [SerializeField] private float _interval = 0.4f;

    private Coroutine _animationCoroutine;

    // Mantém 3 pontos para travar a largura, alternando a opacidade
    private readonly string[] _dotFrames = new string[]
    {
        ".<color=#00000000> . .</color>",   // .
        ". .<color=#00000000> . </color>",  // . .
        ". . ."                             // . . .
    };

    private void OnEnable()
    {
        if (_dotsText != null)
        {
            _animationCoroutine = StartCoroutine(AnimateDotsRoutine());
        }
    }

    private void OnDisable()
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }
    }

    private IEnumerator AnimateDotsRoutine()
    {
        int index = 0;
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(_interval);

        while (true)
        {
            _dotsText.text = _dotFrames[index];
            index = (index + 1) % _dotFrames.Length;
            yield return wait;
        }
    }
}