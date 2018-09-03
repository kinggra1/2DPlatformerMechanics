using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour {

    [Tooltip("Where the health bar for this enemy should be drawn centered.")]
    public GameObject healthBarLocation;
    protected float health = 4f;
    protected float maxHealth = 4f;
    protected AI.Direction direction = AI.Direction.RIGHT;

    // Health bar related variables
    protected Canvas healthCanvas;
    protected Image healthBackgroundImage;
    protected Image healthImage;

    [Tooltip("Scalar from 0 to 1 indicating what percentage of damage is blocked.")]
    public float defense = 1f;

    protected void Start() {
        healthCanvas = healthBarLocation.AddComponent<Canvas>();
        // Creating a canvas gives healthBarLocation object a RectTransform instead of a transform
        RectTransform healthBackgroundRect = healthBarLocation.GetComponent<RectTransform>();
        healthBackgroundRect.sizeDelta = Vector2.one; // 1x1 square (Googled a fair bit of this)
        // Scale to appropriate "health bar" dimensions through scaling the object itself
        healthBarLocation.transform.localScale = new Vector3(1f, 0.15f, 1f);

        // Create a plain white 1x1 pixel texture to use as an image basis.
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);            // Default color is actually gray >:( Make it white for recoloring
        texture.wrapMode = TextureWrapMode.Repeat;      // Repeat this one pixel for any sizing
        texture.Apply();                                // Apply these actual changes to the texture

        healthBackgroundImage = healthBarLocation.AddComponent<Image>();
        healthBackgroundImage.type = Image.Type.Filled;
        healthBackgroundImage.fillMethod = Image.FillMethod.Horizontal;
        healthBackgroundImage.sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), Vector2.zero);
        healthBackgroundImage.color = Color.grey;

        // Add actual healthbar to a child object of the healthBackgroundImage object
        GameObject healthBar = Instantiate(new GameObject("HealthBar"), healthBackgroundImage.transform);
        healthImage = healthBar.AddComponent<Image>();
        RectTransform healthBarRect = healthBar.GetComponent<RectTransform>();
        healthBarRect.sizeDelta = Vector2.one;
        healthImage.type = Image.Type.Filled;
        healthImage.fillMethod = Image.FillMethod.Horizontal; // Make it so we can fill this horizontally like a health bar
        healthImage.sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), Vector2.zero);
        healthImage.color = Color.green;

        // Hide health until somethign else (like taking damage) decides to display it
        healthBarLocation.SetActive(false);
    }

    protected void SetDirection(AI.Direction dir) {
        direction = dir;

        // Update scaling for appropriate local axis directions and sprite reorientation
        Vector3 scale = transform.localScale;
        switch (direction) {
            case AI.Direction.NONE:
                break;
            case AI.Direction.UP:
                break;
            case AI.Direction.DOWN:
                break;
            case AI.Direction.LEFT:
                scale = transform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                transform.localScale = scale;

                scale = healthBarLocation.transform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                healthBarLocation.transform.localScale = scale; // flip the health bar again to keep it correct
                break;
            case AI.Direction.RIGHT:
                scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x);
                transform.localScale = scale;

                scale = healthBarLocation.transform.localScale;
                scale.x = Mathf.Abs(scale.x);
                healthBarLocation.transform.localScale = scale; // flip the health bar again to keep it correct
                break;
        }
    }

    protected void TakeDamage(float damage) {

        health -= damage;
        DisplayHealth();
        if (health <= 0f) {
            Die();
        }
    }

    protected void DisplayHealth() {
        healthBarLocation.SetActive(true);
        healthImage.fillAmount = health / maxHealth;
    }

    private void Die() {
        Destroy(this.gameObject);
    }
}
