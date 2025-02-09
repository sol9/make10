using Freya;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class block : MonoBehaviour
{
    public Image target;
    public TextMeshProUGUI text;

    [Header("resources")]
    public Sprite normal;
    public Sprite highlighted;
    public Sprite disabled;

    private Vector2Int _coord;
    private states _state;
    private int _number;

    public int x
    {
        get => _coord.x;
        set => _coord.x = value;
    }

    public int y
    {
        get => _coord.y;
        set => _coord.y = value;
    }
    
    public states state
    {
        get => _state;
        set
        {
            if (target)
            {
                target.overrideSprite = value switch
                {
                    states.highlighted => highlighted,
                    _ => null,
                };

                target.gameObject.SetActive(value != states.disabled);
            }
            
            if (text)
                text.gameObject.SetActive(value != states.disabled);

            _state = value;
        }
    }
    
    public int number
    {
        get => _number;
        set
        {
            _number = value.Clamp(1, 9);
            
            if (text)
                text.text = _number.ToString();
        }
    }
    
    public enum states
    {
        normal,
        highlighted,
        disabled,
    }

    private void OnValidate()
    {
        if (!target)
            target = GetComponentInChildren<Image>();
        
        if (!text)
            text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void initialize(int x, int y, int number)
    {
        _coord = new Vector2Int(x, y);
        _number = number;
    }
}
