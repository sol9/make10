using System;
using System.Collections.Generic;
using System.Linq;
using Freya;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class game : MonoBehaviour
{
    [Header("prefab")]
    public block blockPrefab;

    [Header("ui")]
    public GridLayoutGroup grid;
    public TextMeshProUGUI timer;
    public TextMeshProUGUI score;
    public Image dragBox;
    
    [Header("config")]
    public Vector2Int gridSize;
    public float gameTime;
    public int goal;
    
    private List<block> _blocks;
    private float _whenStarted;
    private int _score;

    private Vector2 _beginTouched;
    private List<block> _selectedBlocks;

    private void Start()
    {
        reset();
    }

    private void Update()
    {
        var remainingTime = gameTime - (Time.time - _whenStarted);
        if (remainingTime < 0)
        {
            timer.text = "00:00";
            return;
        }

        if (Mouse.current.leftButton.isPressed)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(grid.transform as RectTransform, Mouse.current.position.value, null, out var position);
            
            if (Mouse.current.leftButton.wasPressedThisFrame)
                _onBeginTouch(position);
            else
                _onDrag(position);
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(grid.transform as RectTransform, Mouse.current.position.value, null, out var position);
            _onEndTouch(position);
        }

        timer.text = TimeSpan.FromSeconds(remainingTime).ToString(@"mm\:ss");
    }

    public void reset()
    {
        reset(gridSize.x, gridSize.y, goal);
    }

    public void reset(int width, int height, int goal)
    {
        if (_blocks?.Count > 0)
        {
            _blocks.ForEach(b => Destroy(b.gameObject));
            _blocks = new();
        }

        _score = 0;
        score.text = _score.ToString();
        
        var total = width * height;
        if (total <= 0)
            return;

        if (goal <= 1)
            return;
        
        if (!grid)
            return;

        grid.constraintCount = width;

        var accumulator = 0;
        
        for (var i = 0; i < total; ++i)
        {
            var number = Random.Range(1, goal);

            if (i >= total - 1)
            {
                number = goal - (accumulator % goal);
            }
            else if (i >= total - 2)
            {
                if ((accumulator + number) % goal == 0)
                    number = Random.Range(1, number);
            }

            accumulator += number;
            
            var block = Instantiate(blockPrefab, grid.transform);
            block.x = i % width;
            block.y = i / height;
            block.number = number;
            
            _blocks ??= new();
            _blocks.Add(block);
        }
        
        dragBox.rectTransform.sizeDelta = Vector2.zero;
        
        _whenStarted = Time.time;
        timer.text = TimeSpan.FromSeconds(gameTime).ToString(@"mm\:ss");
    }

    private void _onBeginTouch(Vector2 position)
    {
        if (_selectedBlocks?.Count > 0)
        {
            _selectedBlocks.ForEach(b => b.state = block.states.normal);
            _selectedBlocks = new();
        }

        dragBox.rectTransform.position = position;
        dragBox.rectTransform.sizeDelta = Vector2.zero;
        
        _beginTouched = position;
    }

    private void _onDrag(Vector2 position)
    {
        if (_blocks?.Count <= 0)
            return;

        if (_selectedBlocks?.Count > 0)
            _selectedBlocks.ForEach(b => b.state = block.states.normal);

        var min = new Vector2(Mathfs.Min(_beginTouched.x, position.x), Mathfs.Min(_beginTouched.y, position.y));
        var max = new Vector2(Mathfs.Max(_beginTouched.x, position.x), Mathfs.Max(_beginTouched.y, position.y));
        var bounds = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        _selectedBlocks = _blocks.Where(b => b.state != block.states.disabled && bounds.Contains(b.transform.position)).ToList();
        _selectedBlocks.ForEach(b => b.state = block.states.highlighted);

        dragBox.rectTransform.position = bounds.center;
        dragBox.rectTransform.sizeDelta = bounds.size / dragBox.transform.lossyScale;
    }

    private void _onEndTouch(Vector2 position)
    {
        _onDrag(position);

        if (_selectedBlocks?.Count > 0)
        {
            var sum = _selectedBlocks.Sum(b => b.number);
            if (sum == goal)
            {
                _score += _selectedBlocks.Count;
                score.text = _score.ToString();

                _selectedBlocks.ForEach(b => b.state = block.states.disabled);
            }
            else
            {
                _selectedBlocks.ForEach(b => b.state = block.states.normal);
            }
            
            _selectedBlocks = new();
        }

        dragBox.rectTransform.sizeDelta = Vector2.zero;
    }
}