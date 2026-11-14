using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// Reuses the painted tray without stretching its wells or changing the six-slot rules.
public sealed class MissingSlotTrayEffect : MonoBehaviour
{
    readonly List<Sprite> slices = new();
    readonly List<Transform> anchors = new();
    readonly List<Vector3> anchorPositions = new();
    Transform removedWell, rightCap, cutLine;
    Transform unusedAnchor;
    SpriteRenderer removedRenderer;
    Vector3 capPosition;
    float removedWidth, anchorShift, lineHeight;
    Sequence animation;
    public bool IsComplete { get; private set; }

    public static MissingSlotTrayEffect Create(SpriteRenderer frame, IList<Transform> slots, int activeSlots)
    {
        if (!frame || !frame.sprite || slots == null || activeSlots < 2 || slots.Count <= activeSlots)
            return null;
        var effect = new GameObject("Cut one tray slot", typeof(MissingSlotTrayEffect)).GetComponent<MissingSlotTrayEffect>();
        effect.transform.SetParent(frame.transform, false);
        effect.Build(frame, slots, activeSlots);
        return effect;
    }

    void Build(SpriteRenderer frame, IList<Transform> slots, int count)
    {
        Rect source = frame.sprite.rect;
        Vector2 size = frame.drawMode == SpriteDrawMode.Simple ? (Vector2)frame.sprite.bounds.size : frame.size;
        float pixelWidth = size.x / source.width;
        float pixelHeight = size.y / source.height;
        float lastX = frame.transform.InverseTransformPoint(slots[count - 1].position).x;
        float previousX = frame.transform.InverseTransformPoint(slots[count - 2].position).x;
        float removedX = frame.transform.InverseTransformPoint(slots[count].position).x;
        // Remove an interior well, retaining the entire final well and both original rounded corners.
        // Joining two straight dividers avoids splicing the curved end of a well into the outer cap.
        int cut = Mathf.Clamp(Mathf.RoundToInt(((previousX + lastX) * 0.5f + size.x * 0.5f) / pixelWidth),
            1, (int)source.width - 2);
        int end = Mathf.RoundToInt(((lastX + removedX) * 0.5f + size.x * 0.5f) / pixelWidth);
        int width = Mathf.Clamp(end - cut,
            1, (int)source.width - cut - 1);
        removedWidth = width * pixelWidth;
        MakePart("Tray left body", frame, source, 0, cut, pixelWidth, pixelHeight);
        removedRenderer = MakePart("Cut interior well", frame, source, cut, width, pixelWidth, pixelHeight);
        removedWell = removedRenderer.transform;
        rightCap = MakePart("Original right end and final well", frame, source, cut + width,
            source.width - cut - width, pixelWidth, pixelHeight).transform;
        capPosition = rightCap.localPosition;
        for (int i = 0; i < count; i++)
        {
            anchors.Add(slots[i]);
            anchorPositions.Add(slots[i].localPosition);
        }
        anchorShift = -(anchorPositions[0].x + anchorPositions[count - 1].x) * 0.5f;
        unusedAnchor = slots[count];

        var line = new GameObject("Slot cut light", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
        cutLine = line.transform;
        cutLine.SetParent(transform, false);
        cutLine.localPosition = new Vector3(-size.x * 0.5f + cut * pixelWidth, 0, -0.02f);
        var light = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);
        slices.Add(light);
        line.sprite = light;
        line.color = new Color(1, 0.95f, 0.45f);
        line.sortingLayerID = frame.sortingLayerID;
        line.sortingOrder = frame.sortingOrder + 25;
        lineHeight = size.y * 1.2f;
        cutLine.localScale = new Vector3(0.035f, 0, 1);
        cutLine.gameObject.SetActive(false);
        frame.enabled = false;
    }

    SpriteRenderer MakePart(string name, SpriteRenderer frame, Rect source, float start, float width,
        float pixelWidth, float pixelHeight)
    {
        var renderer = new GameObject(name, typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
        renderer.transform.SetParent(transform, false);
        var sprite = Sprite.Create(frame.sprite.texture, new Rect(source.x + start, source.y, width, source.height),
            Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect);
        slices.Add(sprite);
        renderer.sprite = sprite;
        renderer.sharedMaterial = frame.sharedMaterial;
        renderer.color = frame.color;
        renderer.sortingLayerID = frame.sortingLayerID;
        renderer.sortingOrder = frame.sortingOrder;
        renderer.transform.localScale = new Vector3(100 * pixelWidth, 100 * pixelHeight, 1);
        renderer.transform.localPosition = new Vector3((start + width * 0.5f - source.width * 0.5f) * pixelWidth, 0, 0);
        return renderer;
    }

    public IEnumerator Play()
    {
        if (IsComplete) yield break;
        cutLine.gameObject.SetActive(true);
        animation = DOTween.Sequence().SetLink(gameObject);
        animation.Append(cutLine.DOScaleY(lineHeight, 0.15f).SetEase(Ease.OutQuad));
        animation.AppendInterval(0.08f);
        animation.AppendCallback(() =>
        {
            var burst = TileOnRuntimeFx.CreateBurstParticle(transform, "Slot cut sparks", new Color(1, 0.9f, 0.3f),
                sortingLayerName: removedRenderer.sortingLayerName, sortingOrder: removedRenderer.sortingOrder + 30);
            burst.transform.localPosition = cutLine.localPosition;
            burst.Play();
            Destroy(burst.gameObject, 1.5f);
        });
        animation.Append(removedWell.DOBlendableLocalMoveBy(new Vector3(0.35f, -1.4f, 0), 0.35f).SetEase(Ease.InQuad));
        animation.Join(removedWell.DOLocalRotate(new Vector3(0, 0, -35), 0.35f));
        animation.Join(removedRenderer.DOFade(0, 0.35f));
        animation.Join(cutLine.GetComponent<SpriteRenderer>().DOFade(0, 0.18f));
        animation.Append(rightCap.DOLocalMoveX(capPosition.x - removedWidth, 0.25f).SetEase(Ease.OutCubic));
        animation.Append(transform.DOLocalMoveX(removedWidth * 0.5f, 0.3f).SetEase(Ease.InOutSine));
        foreach (var anchor in anchors)
            animation.Join(anchor.DOLocalMoveX(anchor.localPosition.x + anchorShift, 0.3f).SetEase(Ease.InOutSine));
        yield return animation.WaitForCompletion();
        ApplyFinalLayout();
    }

    public void ApplyFinalLayout()
    {
        rightCap.localPosition = capPosition + Vector3.left * removedWidth;
        transform.localPosition = Vector3.right * (removedWidth * 0.5f);
        removedWell.gameObject.SetActive(false);
        cutLine.gameObject.SetActive(false);
        unusedAnchor.gameObject.SetActive(false);
        for (int i = 0; i < anchors.Count; i++)
            anchors[i].localPosition = anchorPositions[i] + Vector3.right * anchorShift;
        IsComplete = true;
    }

    void OnDestroy()
    {
        animation?.Kill();
        foreach (var sprite in slices)
        {
            if (Application.isPlaying) Destroy(sprite);
            else DestroyImmediate(sprite);
        }
    }
}
