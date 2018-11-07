import korolev.Context
import korolev.execution._
import scala.concurrent.Future
import korolev.state.javaSerialization._

abstract class GuiState()
case class MyItems(isSelling: Boolean = false) extends GuiState
case class MyLots() extends GuiState
case class Auction(isBuying: Boolean = false) extends GuiState

object GuiState {
  val globalContext = Context[Future, GuiState, Any]
}
