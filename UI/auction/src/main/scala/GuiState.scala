import korolev.Context
import korolev.execution._
import scala.concurrent.Future
import korolev.state.javaSerialization._

object GuiState {
  sealed trait GuiState
  final case class MyItems(selectedItem:Int = 0, sellingItem: Option[GameItem] = None) extends GuiState
  final case class MyLots(removingItem: Option[GameItem] = None) extends GuiState
  final case class Auction(buyingItem: Option[GameItem] = None, inProgress: Boolean = false, result: Option[String] = None) extends GuiState

  val globalContext = Context[Future, GuiState, Any]
}
