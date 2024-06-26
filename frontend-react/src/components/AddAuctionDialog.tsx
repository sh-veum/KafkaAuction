import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { useForm } from "@tanstack/react-form";
import { AuctionCreator } from "@/models/AuctionCreator";
import { Spinner } from "./Spinner";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { AlertCircle } from "lucide-react";
import { Textarea } from "@/components/ui/textarea";
import { useInsertAuctionMutation } from "@/utils/mutations/auctionMutations";

export const AddAuctionDialog = () => {
  const { mutate } = useInsertAuctionMutation();

  const form = useForm<AuctionCreator>({
    defaultValues: {
      Title: "",
      Description: "",
      Starting_Price: 0,
      Duration: 1,
    },
    onSubmit: (values) => {
      mutate(values.value);
    },
  });

  return (
    <Dialog>
      <DialogTrigger asChild>
        <Button variant="outline">Add Auction</Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Add Auction</DialogTitle>
          <DialogDescription>Create your own auction</DialogDescription>
        </DialogHeader>
        <form>
          <form.Field
            name="Title"
            validators={{
              onChange: ({ value }) => {
                if (value.length < 3) {
                  return "Title must be at least 3 characters long";
                }
                if (value.length > 40) {
                  return "Title must be at most 40 characters long";
                }
              },
            }}
            children={(field) => (
              <div>
                <Label htmlFor={field.name}>Title</Label>
                <div className="relative">
                  <Input
                    id="Title"
                    type="text"
                    value={field.state.value}
                    onChange={(e) => field.handleChange(e.target.value)}
                  />
                  {/* TODO: Backend validator */}
                  {field.getMeta().isValidating && (
                    <div className="absolute right-2 top-1/2 transform -translate-y-1/2">
                      <Spinner />
                    </div>
                  )}
                </div>
                {field.state.meta.errors && (
                  <div className="text-red-500 text-sm mt-1">
                    {field.state.meta.errors}
                  </div>
                )}
              </div>
            )}
          />
          <form.Field
            name="Description"
            validators={{
              onChange: ({ value }) => {
                if (value.length === 0) {
                  return "Description must not be empty";
                }
                if (value.length > 500) {
                  return "Description must be at most 500 characters long";
                }
              },
            }}
            children={(field) => (
              <div>
                <Label htmlFor={field.name}>Description</Label>
                <div className="relative">
                  <Textarea
                    id="Description"
                    value={field.state.value}
                    onChange={(e) => field.handleChange(e.target.value)}
                  />
                </div>
                {field.state.meta.errors && (
                  <div className="text-red-500 text-sm mt-1">
                    {field.state.meta.errors}
                  </div>
                )}
              </div>
            )}
          />
          <form.Field
            name="Starting_Price"
            validators={{
              onChange: ({ value }) => {
                if (value < 0) {
                  return "Starting price must be greater than 0";
                }
                if (!value) {
                  return "Must enter a starting price";
                }
              },
            }}
            children={(field) => (
              <div>
                <Label htmlFor={field.name}>Starting Price</Label>
                <div className="relative">
                  <Input
                    id="Starting_Price"
                    type="number"
                    step="0.25"
                    value={field.state.value}
                    onChange={(e) =>
                      field.handleChange(parseFloat(e.target.value))
                    }
                  />
                </div>
                {field.state.meta.errors && (
                  <div className="text-red-500 text-sm mt-1">
                    {field.state.meta.errors}
                  </div>
                )}
              </div>
            )}
          />
          <form.Field
            name="Duration"
            validators={{
              onChange: ({ value }) => {
                if (value < 0) {
                  return "Duration must be higher than 0";
                }
                if (!value) {
                  return "Must enter a duration";
                }
              },
            }}
            children={(field) => (
              <div>
                <Label htmlFor={field.name}>Duration (hours)</Label>
                <div className="relative">
                  <Input
                    id="Duration"
                    type="number"
                    value={field.state.value}
                    onChange={(e) =>
                      field.handleChange(parseInt(e.target.value))
                    }
                  />
                </div>
                {field.state.meta.errors && (
                  <div className="text-red-500 text-sm mt-1">
                    {field.state.meta.errors}
                  </div>
                )}
              </div>
            )}
          />
          <form.Subscribe
            selector={(state) => state.errors}
            children={(errors) =>
              errors.length > 0 && (
                <Alert variant={"destructive"}>
                  <AlertCircle className="h-4 w-4" />
                  <AlertTitle>Error</AlertTitle>
                  <AlertDescription>{errors}</AlertDescription>
                </Alert>
              )
            }
          />
        </form>
        <DialogFooter>
          <Button variant="outline" onClick={form.reset}>
            Reset
          </Button>
          <Button
            variant={"ghost"}
            onClick={() => console.log(form.state.values)}
          >
            Debug
          </Button>
          <form.Subscribe
            selector={(state) => [state.canSubmit, state.isValidating]}
            children={([canSubmit, isValidating]) => (
              <Button
                onClick={() => {
                  form.handleSubmit();
                }}
                disabled={!canSubmit || isValidating}
              >
                Create Auction
              </Button>
            )}
          />
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};
