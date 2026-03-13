using UnityEngine;
using System.Collections;

public class PlayerEmote : MonoBehaviour
{
    [SerializeField] private Sprite emoteSprite;
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private float fadeDuration = 0.5f;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {


            // Create emote object
            GameObject emoteObject = new GameObject("Emote_Fade");
            SpriteRenderer sr = emoteObject.AddComponent<SpriteRenderer>();
            sr.sprite = emoteSprite;
            sr.sortingLayerName = "OverPlayer";

            // Position and parent it
            emoteObject.transform.position = other.transform.position + new Vector3(0, 1.5f, 0);
            emoteObject.transform.SetParent(other.transform);

            // Start fade coroutine
            StartCoroutine(FadeAndDestroy(emoteObject, sr));
        }
    }


    private IEnumerator FadeAndDestroy(GameObject obj, SpriteRenderer sr)
    {
        yield return new WaitForSeconds(displayDuration);

        float counter = 0;
        Color startColor = sr.color;

        while (counter < fadeDuration)
        {
            counter += Time.deltaTime;

            float alpha = Mathf.Lerp(1, 0, counter / fadeDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            yield return null;
        }

        Destroy(obj);
    }
}

