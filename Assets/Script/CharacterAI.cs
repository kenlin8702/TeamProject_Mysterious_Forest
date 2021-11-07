using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using System.Collections;
using UnityEngine.UI;

public class CharacterAI : MonoBehaviour
{
    #region 欄位
    private float speed = 300f;
    private float jump = 500f;
    private float JumpCoolDown = 100f;
    [Header("檢查地板位移與半徑")]
    public Vector3 v2GroundOffset;
    public float radiusGround = 10f;
    [Header("音效")]
    public AudioClip soundJump;
    public AudioClip soundLand;
    public AudioClip soundChest;
    public AudioClip soundPass;
    public AudioClip soundDead;
    private Rigidbody2D rig;
    private Animator ani;
    private AudioSource aud;
    private float h;
    private int chatcount = 0;
    private bool[] chatComplete = new bool[2];
    private bool isGrounded;
    private bool dead;
    /// <summary>
    /// 輸入右鍵：D 與 方向右
    /// </summary>
    private bool inputRight
    {
        get
        {
            return Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
        }
    }
    /// <summary>
    /// 輸入左鍵：A 與 方向左
    /// </summary>
    private bool inputLeft
    {
        get
        {
            return Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
        }
    }
    /// <summary>
    /// 吃到寶箱
    /// </summary>
    private bool hasChest;
    /// <summary>
    /// 是否過關
    /// </summary>
    private bool pass;
    /// <summary>
    /// 是否遊戲結束
    /// </summary>
    private bool gameOver;
    /// <summary>
    /// 過關畫面
    /// </summary>
    private CanvasGroup groupGameOver;
    /// <summary>
    /// 重新挑戰
    /// </summary>
    private Button btnReplay;


    /// <summary>
    /// 跳躍按鈕
    /// </summary>
    public bool jumpButton { get; set; }
    #endregion

    #region 事件
    private void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        aud = GetComponent<AudioSource>();
        h = 1f;
        for (int i = 0; i < chatComplete.Length; i++) {
            chatComplete[i] = false;
        }
    }

    private void Update()
    {

        //MoveInput();
        Jump();
        Climb();
        TextBox();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + v2GroundOffset, radiusGround);
    }

    private void MoveInput()
    {
        h = Input.GetAxis("Horizontal");

        if (inputRight) transform.eulerAngles = Vector3.zero;
        else if (inputLeft) transform.eulerAngles = new Vector3(0, 180, 0);


    }
    /// <summary>
    ///移動
    /// </summary>
    private void Move()
    {
        
        rig.velocity = new Vector2(h * speed * Time.fixedDeltaTime, rig.velocity.y);
        if (h != 0f) {
            ani.SetBool("BRun", true);
        } else if(h == 0f && ani.GetBool("BRun")){
            ani.SetBool("BRun", false);
        }
    }

    /// <summary>
    /// 跳躍輸入
    /// </summary>
    private void Jump(){
        Collider2D hitbox = Physics2D.OverlapCircle(this.transform.position + v2GroundOffset, radiusGround, 1 << 9);
        bool hitted = hitbox && hitbox.name.Contains("跳躍觸發");
        JumpCoolDown = Mathf.Min(JumpCoolDown + 1f, 100f);
        if (hitted && isGrounded && JumpCoolDown >= 100f) {
            JumpCoolDown = 0f;
            aud.PlayOneShot(soundJump, Random.Range(0.8f, 1.2f));
            rig.AddForce(new Vector2(0, jump));
        }

        Collider2D hit = Physics2D.OverlapCircle(this.transform.position + v2GroundOffset, radiusGround, 1 << 8);
        if (!isGrounded && hit) aud.PlayOneShot(soundLand, Random.Range(0.8f, 1.2f));
        isGrounded = hit && hit.name.Contains("地板");
        Debug.Log(isGrounded);
        ani.SetBool("BJump", !isGrounded);
    }

    private void Climb() {
        Collider2D hitbox = Physics2D.OverlapCircle(this.transform.position + v2GroundOffset, radiusGround, 1 << 9);
        bool hitted = hitbox && hitbox.name.Contains("爬牆觸發");
        if (hitted) {
            rig.gravityScale = 0f;
            h = 0f;
            rig.velocity = new Vector2(0, 3f);
        } else if (!hitted && rig.gravityScale == 0f) {
            rig.gravityScale = 3f;
            h = 1f;
            rig.AddForce(new Vector2(0, jump));
        }
    }
    private void TextBox()
    {
        Collider2D hitbox = Physics2D.OverlapCircle(this.transform.position + v2GroundOffset, radiusGround, 1 << 9);
        bool hitted = hitbox && hitbox.name.Contains("對話觸發");
        int contactCount = hitbox.contactCount;
        if (contactCount > contacts.Length)
            contacts = new ContactPoint2D[contactCount];
        collision.GetContacts(contacts);
        hitbox.GetComponent<Tilemap>().SetTile(hitbox.GetComponent<Tilemap>().WorldToCell(hitPosition), null);
        if (hitted && h != 0f)
        {
            h = 0f;
        }

        if (h == 0f) {
            chatcount++;
        }
    }
    /// <summary>
    /// 吃東西
    /// </summary>
    /// <param name="collisionName">碰撞物件名稱</param>
    private void Eat(GameObject collisionObj)
    {
        if (collisionObj.name == "寶箱")
        {
            hasChest = true;
            Destroy(collisionObj);
            aud.PlayOneShot(soundChest);
        }
    }


    /// <summary>
    /// 重新遊戲
    /// </summary>
    public void Replay()
    {
        SceneManager.LoadScene(0);
    }
    #endregion
}
