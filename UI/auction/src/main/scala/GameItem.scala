import tethys._
import tethys.jackson._
import tethys.derivation.auto._

case class GameItem (
  id: Long,
  name: String,
  bigImage: String = "https://bulma.io/images/placeholders/640x480.png",
  previewImage: String = "https://bulma.io/images/placeholders/256x256.png"
  )

object GameItem {
  def getItem(id: Int): GameItem = {
    //Get from server
    val itemJson = jsonItems(id)
    val item = itemJson.jsonAs[GameItem]
    item match {
      case Right(x) => x
      case Left(x) => GameItem(-1, "Error")
    }
  }

  val myItems = List(0, 1)
  val myLots = List(2, 3)
  val auction = List(4, 5)

  val jsonItems = Map(
    0 -> "{\"id\":0,\"name\":\"Меч судьбы 0\",\"bigImage\":\"https://game-icons.net/icons/lorc/originals/svg/relic-blade.svg\",\"previewImage\":\"https://game-icons.net/icons/lorc/originals/svg/relic-blade.svg\"}",
    1 -> "{\"id\":1,\"name\":\"Топор\",\"bigImage\":\"https://game-icons.net/icons/delapouite/originals/svg/axe-in-log.svg\",\"previewImage\":\"https://game-icons.net/icons/delapouite/originals/svg/axe-in-log.svg\"}",
    2 -> "{\"id\":2,\"name\":\"Меч судьбы 123\",\"bigImage\":\"https://game-icons.net/icons/lorc/originals/svg/relic-blade.svg\",\"previewImage\":\"https://game-icons.net/icons/lorc/originals/svg/relic-blade.svg\"}",
    3 -> "{\"id\":3,\"name\":\"Лук\",\"bigImage\":\"https://game-icons.net/icons/lorc/originals/svg/pocket-bow.svg\",\"previewImage\":\"https://game-icons.net/icons/lorc/originals/svg/pocket-bow.svg\"}",
    4 -> "{\"id\":4,\"name\":\"Лучший меч ever\",\"bigImage\":\"https://game-icons.net/icons/lorc/originals/svg/broadsword.svg\",\"previewImage\":\"https://game-icons.net/icons/lorc/originals/svg/broadsword.svg\"}",
    5 -> "{\"id\":5,\"name\":\"Ящик\",\"bigImage\":\"https://game-icons.net/icons/delapouite/originals/svg/wooden-crate.svg\",\"previewImage\":\"https://game-icons.net/icons/delapouite/originals/svg/wooden-crate.svg\"}",
  )
}
