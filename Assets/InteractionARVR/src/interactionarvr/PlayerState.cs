namespace me.buhlmann.study.ARVR {
  public enum PlayerState : byte {
    DEFAULT = 0x00,
    DEFAULT_CAN_INTERACT = 0x01,
    MOVEMENT = 0x02,
    INTERACT_TRANSLATE = 0x03,
    INTERACT_ROTATE = 0x04
  }
}