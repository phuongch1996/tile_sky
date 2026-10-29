using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class TMPArcText : MonoBehaviour
{
    [SerializeField] private float curveHeight = 30f;

    private TMP_Text tmpText;

    private void OnEnable()
    {
        tmpText = GetComponent<TMP_Text>();
        tmpText.OnPreRenderText += BendText;
        Refresh();
    }

    private void OnDisable()
    {
        if (tmpText != null)
            tmpText.OnPreRenderText -= BendText;
    }

    private void OnValidate()
    {
        if (!isActiveAndEnabled)
            return;

        tmpText = GetComponent<TMP_Text>();
        Refresh();
    }

    private void Refresh()
    {
        if (tmpText == null)
            return;

        tmpText.havePropertiesChanged = true;
        tmpText.ForceMeshUpdate();
    }

    private void BendText(TMP_TextInfo textInfo)
    {
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo character = textInfo.characterInfo[i];

            if (!character.isVisible)
                continue;

            int materialIndex = character.materialReferenceIndex;
            int vertexIndex = character.vertexIndex;

            Vector3[] vertices =
                textInfo.meshInfo[materialIndex].vertices;

            TMP_LineInfo line =
                textInfo.lineInfo[character.lineNumber];

            float left = line.lineExtents.min.x;
            float right = line.lineExtents.max.x;
            float halfWidth = (right - left) * 0.5f;

            if (halfWidth <= 0.001f)
                continue;

            float lineCenter = (left + right) * 0.5f;

            Vector3 charCenter =
                (vertices[vertexIndex] +
                 vertices[vertexIndex + 2]) * 0.5f;

            float normalizedX =
                (charCenter.x - lineCenter) / halfWidth;

            float yOffset =
                curveHeight * (1f - normalizedX * normalizedX);

            float slope =
                -2f * curveHeight * normalizedX / halfWidth;

            float angle =
                Mathf.Atan(slope) * Mathf.Rad2Deg;

            Quaternion rotation =
                Quaternion.Euler(0f, 0f, angle);

            for (int j = 0; j < 4; j++)
            {
                int index = vertexIndex + j;

                Vector3 localVertex =
                    vertices[index] - charCenter;

                vertices[index] =
                    rotation * localVertex
                    + charCenter
                    + Vector3.up * yOffset;
            }
        }

        tmpText.UpdateVertexData(
            TMP_VertexDataUpdateFlags.Vertices
        );
    }
}