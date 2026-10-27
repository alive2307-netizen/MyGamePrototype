using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float speedBoostMultiplier = 2f;
    private float currentSpeed;
    private float boostTimer = 0f;

    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Score")]
    private int score = 0;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI healthText;

    [Header("Mobile Controls")]
    public VirtualJoystick joystick;
    public bool useGyroscope = false; // Галочка для включения гироскопа

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        currentSpeed = moveSpeed;
        
        UpdateUI(); 
        
        // Включение гироскопа, если галочка стоит
        if (useGyroscope && SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            Debug.Log("Гироскоп включен");
        }
        
        Debug.Log("Игра началась. Здоровье: " + currentHealth + ", Счёт: " + score);
    }

    void Update()
    {
        // 1. Ввод с клавиатуры
        float x = 0f;
        float y = 0f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y = -1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x = 1f;
        Vector2 keyboardInput = new Vector2(x, y).normalized;

        // 2. Ввод с джойстика
        Vector2 joystickInput = Vector2.zero;
        if (joystick != null) joystickInput = joystick.GetInput();

        // 3. Ввод с гироскопа
        Vector2 gyroInput = Vector2.zero;
        if (useGyroscope && SystemInfo.supportsGyroscope)
        {
            // Берем наклон телефона по осям X и Y (упрощенно)
            gyroInput.x = Input.gyro.rotationRateUnbiased.y * -0.5f;
            gyroInput.y = Input.gyro.rotationRateUnbiased.x * 0.5f;
            
            // Ограничиваем, чтобы не улететь за пределы -1..1
            gyroInput = Vector2.ClampMagnitude(gyroInput, 1f);
        }

        // 4. Выбираем приоритетный ввод
        if (useGyroscope && gyroInput.magnitude > 0.1f)
        {
            moveInput = gyroInput;
        }
        else if (joystickInput.magnitude > 0.1f)
        {
            moveInput = joystickInput;
        }
        else
        {
            moveInput = keyboardInput;
        }

        // Таймер ускорения
        if (boostTimer > 0)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0)
            {
                currentSpeed = moveSpeed;
                Debug.Log("Ускорение закончилось. Скорость: " + currentSpeed);
            }
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * currentSpeed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Score"))
        {
            score += 10;
            Debug.Log("Очки! Счёт: " + score);
            Destroy(other.gameObject);
            UpdateUI(); 
        }
        else if (other.CompareTag("Damage"))
        {
            currentHealth -= 1;
            Debug.Log("Урон! Здоровье: " + currentHealth);
            Destroy(other.gameObject);
            UpdateUI(); 
            
            if (currentHealth <= 0)
            {
                Debug.Log("Игра окончена. Перезапуск...");
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
        }
        else if (other.CompareTag("Speed"))
        {
            currentSpeed = moveSpeed * speedBoostMultiplier;
            boostTimer = 3f;
            Debug.Log("Ускорение! Скорость: " + currentSpeed + " на 3 секунды");
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Health"))
        {
            currentHealth = Mathf.Min(currentHealth + 1, maxHealth);
            Debug.Log("Здоровье! Теперь: " + currentHealth);
            Destroy(other.gameObject);
            UpdateUI(); 
        }
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Счёт: " + score;
        if (healthText != null) healthText.text = "Жизни: " + currentHealth;
    }
}

// Движение через джойстик и клавиатуру