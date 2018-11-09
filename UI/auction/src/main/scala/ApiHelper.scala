import akka.actor.ActorSystem
import akka.stream.ActorMaterializer
import com.google.protobuf.ByteString
import pravda.node.client.impl._
import pravda.common.bytes
import pravda.common.domain.NativeCoin
import pravda.vm.asm.PravdaAssembler

import scala.concurrent.ExecutionContextExecutor

object ApiHelper{

  private val privateKey = ""

  def callMethod(address: String, methodName: String, args: List[Any])(implicit system: ActorSystem,
                                      materializer: ActorMaterializer,
                                      executionContext: ExecutionContextExecutor) = {

    val allArgs = args :: List("\"" + methodName + "\"", address, args.length + 2)

    val asm = allArgs.mkString("push ", " push ", " pcall")

    val assembled = PravdaAssembler.assemble(asm, saveLabels = true)

    assembled match {
      case Right(data) =>
        val impl = new NodeLanguageImpl
        impl.singAndBroadcastTransaction(
          "",
          bytes.hex2byteString(address),
          bytes.hex2byteString(privateKey),
          None,
          10000,
          NativeCoin(0L),
          None,
          data
        )
      // case Left(error) =>
    }
    pravda.node.data.
  }
}