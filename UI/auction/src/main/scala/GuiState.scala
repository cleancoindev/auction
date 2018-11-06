import korolev.Context
import korolev.execution._
import scala.concurrent.Future
import korolev.state.javaSerialization._

abstract class GuiState()
case class MyItems() extends GuiState
case class MyLots() extends GuiState
case class Auction() extends GuiState

object GuiState {
  val globalContext = Context[Future, GuiState, Any]
}
