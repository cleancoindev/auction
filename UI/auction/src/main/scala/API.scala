import akka.actor.ActorSystem
import akka.stream.ActorMaterializer
import pravda.vm.Data.Primitive._

import scala.concurrent.{ExecutionContextExecutor, Future}

class API(implicit system: ActorSystem,
          materializer: ActorMaterializer,
          executionContext: ExecutionContextExecutor) {
  private val apiHelper: ApiHelper = new ApiHelper()

  def getMeta: () => Future[Either[String, Utf8]] = apiHelper.apiSimple0[Utf8](address = "fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b", methodName = "getMeta")
  def test: Utf8 => Future[Either[String, Utf8]] = apiHelper.apiSimple1[Utf8, Utf8](address = "fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b", methodName = "test")
}
