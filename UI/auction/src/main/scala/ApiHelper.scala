import akka.actor.{ActorSystem, FSM}
import akka.actor.Status.Success
import akka.stream.ActorMaterializer
import com.google.protobuf.ByteString
import pravda.node.client.impl._
import pravda.common.bytes
import pravda.common.domain.NativeCoin
import pravda.node.servers.Abci
import pravda.vm.asm.{Operation, PravdaAssembler}
import pravda.node.data.serialization._
import pravda.node.data.serialization.json._
import pravda.vm.Data
import pravda.vm.Data.Primitive._
import pravda.vm.asm.Operation._
import pravda.vm.Opcodes

import scala.concurrent.{ExecutionContextExecutor, Future}

object ApiHelper {

  private val pAddress = "e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342"
  private val privateKey = "99a3039a04662e37686dd1b7a181ca2e4a94a7bf12ce8ee5cf78603f4b8dd4d3e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342"

  def callMethod(address: String, methodName: String, args: List[Data.Primitive])(implicit system: ActorSystem,
                                                                                  materializer: ActorMaterializer,
                                                                                  executionContext: ExecutionContextExecutor): Future[Either[String, Abci.TransactionResult]] = {

    val argsOps = args map {
      arg => Push(arg)
    }

    val ops = argsOps ++ Seq(
      Push(Utf8(methodName)),
      Push(Bytes(bytes.hex2byteString(address))),
      Orphan(Opcodes.PCALL)
    )

    val assembled = PravdaAssembler.assemble(ops, saveLabels = true)

    val impl = new NodeLanguageImpl
    println("broadcasting...")
    val result = try {
      impl.singAndBroadcastTransaction(
        "http://localhost:8087/api/public/broadcast",
        bytes.hex2byteString(pAddress),
        bytes.hex2byteString(privateKey),
        None,
        100000,
        wattPrice = NativeCoin @@ 1L,
        None,
        assembled
      )
    } catch {
      case e: Throwable =>
        e.printStackTrace()
        Future.failed(e)
    }
    println("done")
    result.andThen{
      case x =>
        println(x)
    }
  }

  def getMeta(id : Int)(implicit system: ActorSystem,
                        materializer: ActorMaterializer,
                        executionContext: ExecutionContextExecutor) : Future[Either[String, String]] = {
    //val idD = Data(id)
    val result = callMethod("xfb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b", "getMeta", Nil)
    result map {
      case Right(txResult) =>
        txResult.executionResult match {
          case Right(state) =>
            state.stack.head match {
              case Utf8(meta) => Right(meta)
              case _ => Left("Unknown stack value")
            }
          case Left(runtimeException) =>
            println(runtimeException.finalState.stack.head.mkString())
            Left("RuntimeException code: " + runtimeException.error.code.toString)
        }
      case Left(error) =>
        Left(error)
    }
  }
}