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
import pravda.vm.Data.Primitive
import pravda.vm.Data.Primitive._
import pravda.vm.asm.Operation._
import pravda.vm.Opcodes

import tethys._
import tethys.jackson._

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
      Push(Int32(argsOps.length + 1)),
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
}

class ApiHelper()(implicit system: ActorSystem,
                materializer: ActorMaterializer,
                executionContext: ExecutionContextExecutor) {

  private def apiSimpleCall[R <: Primitive : Manifest](address: String, methodName: String, args: List[Primitive]): Future[Either[String, R]] = {
    val callResult = ApiHelper.callMethod(address, methodName, args)
    callResult map {
      case Right(txResult) =>
        txResult.executionResult match {
          case Right(state) =>
            state.stack.head match {
              case result: R => Right(result)
              case _ => Left("Unknown stack value")
            }
          case Left(runtimeException) =>
            Left("RuntimeException code: " + runtimeException.error.code.toString)
        }
      case Left(error) =>
        Left(error)
    }
  }

  def apiSimple[R <: Primitive : Manifest](address: String, methodName: String): List[Primitive] => Future[Either[String, R]] = {
    dataList => apiSimpleCall(address, methodName, dataList)
  }

  def apiSimple0[R <: Primitive : Manifest](address: String, methodName: String): () => Future[Either[String, R]] = {
    () => apiSimpleCall(address, methodName, Nil)
  }

  def apiSimple1[T <: Primitive, R <: Primitive : Manifest](address: String, methodName: String): T => Future[Either[String, R]] = {
    t => apiSimpleCall(address, methodName, List(t))
  }

  def apiSimple2[T1 <: Primitive, T2 <: Primitive, R <: Primitive : Manifest](address: String, methodName: String): (T1, T2) => Future[Either[String, R]] = {
    (t1, t2) => apiSimpleCall(address, methodName, List(t1, t2))
  }

  def apiSimple3[T1 <: Primitive, T2 <: Primitive, T3 <: Primitive, R <: Primitive : Manifest](address: String, methodName: String): (T1, T2, T3) => Future[Either[String, R]] = {
    (t1, t2, t3) => apiSimpleCall(address, methodName, List(t1, t2, t3))
  }

  def apiSimple4[T1 <: Primitive, T2 <: Primitive, T3 <: Primitive, T4 <: Primitive, R <: Primitive : Manifest](address: String, methodName: String): (T1, T2, T3, T4) => Future[Either[String, R]] = {
    (t1, t2, t3, t4) => apiSimpleCall(address, methodName, List(t1, t2, t3, t4))
  }

  def apiSimple5[T1 <: Primitive, T2 <: Primitive, T3 <: Primitive, T4 <: Primitive, T5 <: Primitive, R <: Primitive : Manifest](address: String, methodName: String): (T1, T2, T3, T4, T5) => Future[Either[String, R]] = {
    (t1, t2, t3, t4, t5) => apiSimpleCall(address, methodName, List(t1, t2, t3, t4, t5))
  }

  private def apiGetClassCall[R](address: String, methodName: String, args: List[Primitive])(implicit jsonReader: JsonReader[R]): Future[Either[String, R]] = {
    val callResult = ApiHelper.callMethod(address, methodName, args)
    callResult map {
      case Right(txResult) =>
        txResult.executionResult match {
          case Right(state) =>
            state.stack.head match {
              case Utf8(result) =>
                val parsed = result.jsonAs[R]
                parsed match {
                  case Right(instance) => Right(instance)
                  case Left(error) => Left(error.getMessage)
                }
              case _ => Left("Unknown stack value")
            }
          case Left(runtimeException) =>
            Left("RuntimeException code: " + runtimeException.error.code.toString)
        }
      case Left(error) =>
        Left(error)
    }
  }

  def apiGetClass[R](address: String, methodName: String)(implicit jsonReader: JsonReader[R]): List[Primitive] => Future[Either[String, R]] = {
    dataList => apiGetClassCall(address, methodName, dataList)(jsonReader)
  }

  def apiGetClass0[R](address: String, methodName: String)(implicit jsonReader: JsonReader[R]): () => Future[Either[String, R]] = {
    () => apiGetClassCall(address, methodName, Nil)(jsonReader)
  }

  def apiGetClass1[T <: Primitive, R](address: String, methodName: String)(implicit jsonReader: JsonReader[R]): T => Future[Either[String, R]] = {
    t => apiGetClassCall(address, methodName, List(t))(jsonReader)
  }

  def apiGetClass2[T1 <: Primitive, T2 <: Primitive, R](address: String, methodName: String)(implicit jsonReader: JsonReader[R]): (T1, T2) => Future[Either[String, R]] = {
    (t1, t2) => apiGetClassCall(address, methodName, List(t1, t2))(jsonReader)
  }

  def apiGetClass3[T1 <: Primitive, T2 <: Primitive, T3 <: Primitive, R](address: String, methodName: String)(implicit jsonReader: JsonReader[R]): (T1, T2, T3) => Future[Either[String, R]] = {
    (t1, t2, t3) => apiGetClassCall(address, methodName, List(t1, t2, t3))(jsonReader)
  }

  def apiGetClass4[T1 <: Primitive, T2 <: Primitive, T3 <: Primitive, T4 <: Primitive, R](address: String, methodName: String)(implicit jsonReader: JsonReader[R]): (T1, T2, T3, T4) => Future[Either[String, R]] = {
    (t1, t2, t3, t4) => apiGetClassCall(address, methodName, List(t1, t2, t3, t4))(jsonReader)
  }

  def apiGetClass5[T1 <: Primitive, T2 <: Primitive, T3 <: Primitive, T4 <: Primitive, T5 <: Primitive, R](address: String, methodName: String)(implicit jsonReader: JsonReader[R]): (T1, T2, T3, T4, T5) => Future[Either[String, R]] = {
    (t1, t2, t3, t4, t5) => apiGetClassCall(address, methodName, List(t1, t2, t3, t4, t5))(jsonReader)
  }
}